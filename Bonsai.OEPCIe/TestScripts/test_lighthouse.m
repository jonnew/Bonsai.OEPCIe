%% Visualize raw data
cutoffs = [50, 62.5,72.9,83.3,93.8,104,115,125,135] + 5;

close all
figure

for idx = 1:4
    %% Load raw data
    fid = fopen(['lh' num2str(idx) '.csv']);
    x = textscan(fid,'%f %f %d');

    time = x{1} / 50e6;
    width = x{2} / 50e6;
    type = x{3};

    %% Pulse widths

    hold all
    plot(1e6 * width, '.')

end

plot([0, length(width)],[cutoffs; cutoffs], 'k--')


%% Constants
period = 1 / 60;

% Base-station positions
p0 = [0; 0; 0];
q0 = [-0.63; 0; 0];

% Extrinsic rotations of the lighthouses (Z, X, Z)
eul0 = [-pi/2, - pi/2, 3 * pi/4];
eul1 = [pi/2, - pi/2, -3 * pi/4];
% R0 = eul2rotm(eul0, 'ZXZ');
% R1 = eul2rotm(eul1, 'ZXZ');
R0 = eye(3);
R1 = eye(3);

% Full round robin sequence of [bad, skip, ax, sweep] across both
% base-stations
template = [
 0 0 0 0;
 0 1 0 0;
 0 0 0 1; % ax 0, station 0
 0 0 1 0;
 0 1 1 0;
 0 0 0 1; % ax 1, station 0
 0 1 0 0;
 0 0 0 0;
 0 0 0 1; % ax 0, station 1
 0 1 1 0;
 0 0 1 0;
 0 0 0 1; % ax 1, station 1
 ];

%% Cycle thru diode of interest
pos_t = cell(4, 1);
pos =  cell(4, 1);

for diode = 1:4
    
    fid = fopen(['lh' num2str(diode) '.csv']);
    x = textscan(fid,'%f %f %d');

    time = x{1} / 50e6;
    width = x{2} / 50e6;
    type = x{3};

    %% Interpret pulses to get angles
    % Every sweep is preceed by two sync flashes that encode the ax that the
    % sweep will encode and the wether the rotor will skip
    % Each base station gets a turn, starting with ax 1 and then 0. 

    bad = type == -1;
    sweep = type == 8;
    skip = type >= 4 & type ~= 8;
    ax = mod(type, 2) == 1 & type ~= 8;
    bsas = [bad, skip, ax, sweep];

    % Look for two syncs above each sweep
    swp_idx = find(sweep);

    % Inefficient -- should only cycle through sweep indicies
    for i = size(template,1) : size(bsas, 1)

        % Pull chunk
        x = bsas(i - size(template,1) + 1: i, :);

        % If matches template
        if (isequal(x, template))

            idx = i - size(template,1) + 1: i;

            % Sanity checkking on times of pulses in template
            % make sure time between pulses does not exceed a period
            t = time(idx);
            if (sum(abs(diff(t)) > (period / 2) ))
                continue;
            end

            % Mean time of first and last pulse
            sample_t = (time(idx(1)) + time(idx(11))) / 2;

            % Station 0
            t00 = time(idx(3)) + width(idx(3))/2 - time(idx(1));
            t10 = time(idx(6)) + width(idx(6))/2 - time(idx(4));
            theta0 = 2 * pi * (t00 / period) - pi / 2;
            gamma0 = 2 * pi * (t10 / period) - pi / 2;

    %         u = [sin(theta0) * cos(gamma0); 
    %         	  sin(theta0) * sin(gamma0);
    %               cos(gamma0)              ];

            u = [tan(theta0);  tan(gamma0); 1];
            u = u / norm(u);
    %         u = R0 * u;


            % Station 1
            t01 = time(idx(9)) + width(idx(9))/2 - time(idx(8));
            t11 = time(idx(12))+ width(idx(12))/2 - time(idx(11));
            theta1 = 2 * pi * (t01 / period) - pi / 2;
            gamma1 = 2 * pi * (t11 / period) - pi / 2;
    %         v = [sin(theta1) * cos(gamma1); 
    %         	  sin(theta1) * sin(gamma1);
    %               cos(gamma1)              ];

            v = [tan(theta1);  tan(gamma1); 1];
            v = v /norm(v);
            %         v = R1 * v;


            d = q0 - p0;
            r = [[1, -dot(u, v)];[ dot(u, v), -1]] \ [dot(u, d); dot(v, d)];

            p1 = p0 + r(1) * u;
            q1 = q0 + r(2) * v;
            p = (p1 + q1)/2;
            
            
            % Ad hoc smoothness criteria
            if ~isempty(pos_t{diode}) && norm(p - pos{diode}(end, :)') > .5
               continue; 
            end
            
            pos_t{diode} = [pos_t{diode}; sample_t];
            pos{diode} = [pos{diode}; p'];
            
            

%             % Plot the vectors and intersections
%             clf
%             hold all
%             plot3([p0(1) q0(1)], [p0(2) q0(2)], [p0(3) q0(3)], 'k--')
%             plot3(p0(1), p0(2), p0(3),'r.')
%             plot3([p0(1) p0(1) + 10*u(1)], [p0(2) p0(2) + 10*u(2)], [p0(3) p0(3) + 10*u(3)],'r-')
%             plot3(p1(1),p1(2),p1(3), 'r*')
% 
% 
%             plot3(q0(1), q0(2), q0(3),'b.');
%             plot3([q0(1) q0(1) + 10*v(1)], [q0(2) q0(2) + 10*v(2)], [q0(3) q0(3) + 10*v(3)],'b-')
%             plot3(q1(1),q1(2),q1(3), 'b*')
% 
%             plot3(p(1),p(2),p(3), 'g*')
% 
%             axis([-2 2 -2 2 -0.5 2])
% 
%             xlabel('x')
%             ylabel('y')
%             zlabel('z')
%             view(3); %[0 0])
%             drawnow
        end
    end

end



%% Angles
% clf; plot(angle./pi);
% legend('theta0', 'gamma0', 'theta1', 'gamma1');
% ylim([-.5 .5]*pi)


%%
figure
subplot(221)
hold all
for i = 1: numel(pos)
    scatter(pos{i}(:,1), pos{i}(:,2), '.')
end
axis([-1 1 -1 1])
xlabel('x')
ylabel('y')
daspect([1 1 1]);

subplot(222)
hold all
for i = 1: numel(pos)
    scatter(pos{i}(:,1), pos{i}(:,3), '.')
end
axis([-1 1 0 2])
xlabel('x')
ylabel('z')
daspect([1 1 1]);

subplot(223)
hold all
for i = 1: numel(pos)
    scatter(pos{i}(:,2), pos{i}(:,3), '.')
end
axis([-1 1 0 2])
xlabel('y')
ylabel('z')
daspect([1 1 1]);

subplot(224)
hold all
for i = 1: numel(pos)
    plot3(pos{i}(:,1), pos{i}(:,2), pos{i}(:,3), '.')
end
daspect([1 1 1]);
view(3)

%% Interpolate positions across time
close all
% Find min/max of pos time
min_t =  min(pos_t{i});
max_t = max(pos_t{i});
tq = min_t:period:max_t;

% Find gaps in data and remove
thresh = 0.5;
for i = 1: numel(pos)
    bad_idx = find(diff(pos_t{i}) > thresh);
    
    for j = 1: numel(bad_idx)
       tq(tq >= pos_t{i}(bad_idx(j)) & tq <= pos_t{i}(bad_idx(j) + 1)) = [];
    end
end

pos_1 = interp1(pos_t{1}, pos{1}, tq);
pos_2 = interp1(pos_t{2}, pos{2}, tq);
pos_3 = interp1(pos_t{3}, pos{3}, tq);
pos_4 = interp1(pos_t{4}, pos{4}, tq);


%% Get vectors normal to headstage plane
n_vec = zeros(numel(tq), 3);
f_vec = zeros(numel(tq), 3);
h_vec = zeros(numel(tq), 3);
% vecs2 =  zeros(numel(tq), 3);
mids = zeros(numel(tq), 3);
u_det = [];
for i = 1: numel(tq)
   
    % SVD method
    X = [pos_1(i, :)', pos_2(i, :)', pos_3(i, :)', pos_4(i, :)'];
    mids(i, :) = mean(X, 2)';
    X = X - mean(X, 2);
    
    if  sum(isnan(X(:)))
        n_vec(i, :) = nan(1,3);
        f_vec(i, :) = nan(1,3);
        h_vec(i, :) = nan(1,3);
    else
        [U, S, V] = svd(X);
        u_det = [u_det; det(U)];
        f_vec(i, :) = U(:, 1)';
        h_vec(i, :) = U(:, 2)';
        n_vec(i, :) = U(:, 3)';
    end
    
%     % cross product
%     v1 = pos_2(i, :) - pos_1(i, :);
%     v2 = pos_3(i, :) - pos_1(i, :);
%     vecs2(i,:) = cross(v1, v2);
%     vecs2(i,:) = vecs2(i,:)/ norm(vecs2(i,:));
end

% hold all
% plot(pos_1)
% plot(pos_2)
% plot(pos_3)
% plot(pos_4)

%% Summary

% Position time series
close all
figure

subplot(121)
for i = 1: numel(pos)
    plot(pos_t{i}, pos{i}, '.');
end
legend('x', 'y', 'z');
xlabel('time (sec)')
ylabel('distance (m)')

subplot(122)
hold on;

view(3)
xlabel('x (m)')
ylabel('y (m)')
zlabel('z (m)')
daspect([1 1 1]);

for i = 1 : numel(tq)
%     dirvec=(pos_1(i,:)-pos_2(i,:)).*5;
    dirvec = n_vec(i, :) * 0.01;
    plot3([mids(i, 1), mids(i, 1)+dirvec(1)],[mids(i, 2), mids(i, 2)+dirvec(2)], [mids(i, 3),mids(i, 3)+dirvec(3)],'k');
    
    dirvec = h_vec(i, :) * 0.01;
    plot3([mids(i, 1), mids(i, 1)+dirvec(1)],[mids(i, 2), mids(i, 2)+dirvec(2)], [mids(i, 3),mids(i, 3)+dirvec(3)],'b');
    
    dirvec = f_vec(i, :) * 0.01;
    plot3([mids(i, 1), mids(i, 1)+dirvec(1)],[mids(i, 2), mids(i, 2)+dirvec(2)], [mids(i, 3),mids(i, 3)+dirvec(3)],'r');
    
    plot3([mids(i, 1)],[mids(i, 2)], [mids(i, 3)],'k.')
    
%     pgon = polyshape([0 0 1 1],[1 0 0 1]);
%     poly = [[pos_1(i, 1), pos_2(i, 1), pos_3(i, 1), pos_4(i, 1), pos_1(i, 1)];
%             [pos_1(i, 2),pos_2(i, 2), pos_3(i, 2), pos_4(i, 2), pos_1(i, 2)];
%             [pos_1(i, 3),pos_2(i, 3), pos_3(i, 3), pos_4(i, 3), pos_1(i, 3)]];
%     
%     fill3(poly(1, :), poly(2, :), poly(3, :), [1 1 1 1 0])
%     plot3(poly,'r');
    
%     plot3([pos_1(i, 1),pos_2(i, 1), pos_3(i, 1), pos_4(i, 1), pos_1(i, 1)], ...
%         [pos_1(i, 2),pos_2(i, 2), pos_3(i, 2), pos_4(i, 2), pos_1(i, 2)],...
%         [pos_1(i, 3),pos_2(i, 3), pos_3(i, 3), pos_4(i, 3), pos_1(i, 3)]);
%     
%     drawnow;
end










